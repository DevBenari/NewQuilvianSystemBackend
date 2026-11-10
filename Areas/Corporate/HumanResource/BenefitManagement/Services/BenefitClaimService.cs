using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Layanan Pengajuan Klaim Koordinasi Manfaat (COB), Settlement & Kliring Exit Clearance (Slice S-D3, BE-HRD-188 & 189).
    /// Mengimplementasikan penegakan aturan:
    /// - BEN-B5 & VR-BEN-16..17: Mandatory nomor SEP BPJS Kesehatan 19 karakter untuk klaim COB rawat inap.
    /// - VR-BEN-18: Konsistensi rincian biaya: Total = Primer + Sekunder + Ekses.
    /// - VR-BEN-19: Batas waktu kedaluwarsa klaim maksimal 60 hari kalender.
    /// - VR-BEN-20..21: Mandatory kuitansi asli dan ukuran file maks 5 MB.
    /// - BEN-B7 & VR-BEN-23: Penonaktifan benefit pada tanggal efektif keluar.
    /// - BEN-B8 & VR-BEN-24: Blokir persetujuan Exit Clearance Sub-20 bila ada tunggakan ekses medis di RS.
    /// </summary>
    public sealed class BenefitClaimService : IBenefitClaimService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<BenefitClaimService> _logger;
        private readonly NumberSeriesAllocator _numberAllocator;

        private static readonly Regex SepRegex = new(@"^[a-zA-Z0-9]{19}$", RegexOptions.Compiled);

        public BenefitClaimService(ApplicationDbContext dbContext, ILogger<BenefitClaimService> logger, NumberSeriesAllocator numberAllocator)
        {
            _dbContext = dbContext;
            _logger = logger;
            _numberAllocator = numberAllocator;
        }

        public async Task<PagedResult<BenefitClaimSummaryDto>> GetClaimsPagedAsync(
            BenefitClaimListQuery query, CancellationToken cancellationToken)
        {
            var baseQuery = _dbContext.TrxBenefitClaims.AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(query.ClaimStatus))
            {
                baseQuery = baseQuery.Where(x => x.ClaimStatus == query.ClaimStatus);
            }

            if (!string.IsNullOrWhiteSpace(query.ServiceType))
            {
                baseQuery = baseQuery.Where(x => x.ServiceType == query.ServiceType);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchKeyword))
            {
                var kw = query.SearchKeyword.Trim().ToLower();
                baseQuery = baseQuery.Where(x =>
                    x.ClaimNumber.ToLower().Contains(kw) ||
                    (x.SepBpjsNumber != null && x.SepBpjsNumber.ToLower().Contains(kw)) ||
                    (x.HospitalName != null && x.HospitalName.ToLower().Contains(kw)) ||
                    (x.Employee != null && x.Employee.FullName.ToLower().Contains(kw)) ||
                    (x.WorkforceProfile != null && x.WorkforceProfile.DisplayName.ToLower().Contains(kw)));
            }

            var totalItems = await baseQuery.CountAsync(cancellationToken);

            var items = await baseQuery
                .OrderByDescending(x => x.SubmittedAt ?? x.CreateDateTime)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new BenefitClaimSummaryDto
                {
                    Id = x.Id,
                    ClaimNumber = x.ClaimNumber,
                    EmployeeCode = x.Employee != null ? x.Employee.EmployeeNumber : (x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : "-"),
                    EmployeeName = x.Employee != null ? x.Employee.FullName : (x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : "-"),
                    PatientName = x.Employee != null ? x.Employee.FullName : (x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : "-"),
                    PatientType = x.DependentId.HasValue ? "Dependent" : "Employee",
                    ServiceType = x.ServiceType ?? x.ClaimCategory ?? "Inpatient",
                    HospitalName = x.HospitalName ?? x.ProviderName ?? "-",
                    SepBpjsNumber = x.SepBpjsNumber,
                    ServiceDate = x.ServiceStartDate ?? x.ClaimDate,
                    TotalHospitalBill = x.TotalHospitalBill > 0 ? x.TotalHospitalBill : x.ClaimedAmount,
                    PrimaryPayerCoveredAmount = x.PrimaryPayerCoveredAmount,
                    SecondaryClaimAmount = x.SecondaryClaimAmount > 0 ? x.SecondaryClaimAmount : x.ClaimedAmount,
                    ApprovedAmount = x.ApprovedAmount,
                    ClaimStatus = x.ClaimStatus,
                    SubmittedAt = x.SubmittedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<BenefitClaimSummaryDto>
            {
                PageNumber = query.Page,
                PageSize = query.PageSize,
                TotalData = totalItems,
                TotalPage = (int)Math.Ceiling((double)totalItems / query.PageSize),
                Items = items
            };
        }

        public async Task<List<BenefitClaimSummaryDto>> GetMyClaimsAsync(
            Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var claims = await _dbContext.TrxBenefitClaims.AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.SubmittedAt ?? x.CreateDateTime)
                .ToListAsync(cancellationToken);

            return claims.Select(x => new BenefitClaimSummaryDto
            {
                Id = x.Id,
                ClaimNumber = x.ClaimNumber,
                EmployeeCode = x.WorkforceProfile?.ProfileCode ?? "-",
                EmployeeName = x.WorkforceProfile?.DisplayName ?? "-",
                PatientName = x.WorkforceProfile?.DisplayName ?? "-",
                PatientType = x.DependentId.HasValue ? "Dependent" : "Employee",
                ServiceType = x.ServiceType ?? x.ClaimCategory ?? "Inpatient",
                HospitalName = x.HospitalName ?? x.ProviderName ?? "-",
                SepBpjsNumber = x.SepBpjsNumber,
                ServiceDate = x.ServiceStartDate ?? x.ClaimDate,
                TotalHospitalBill = x.TotalHospitalBill > 0 ? x.TotalHospitalBill : x.ClaimedAmount,
                PrimaryPayerCoveredAmount = x.PrimaryPayerCoveredAmount,
                SecondaryClaimAmount = x.SecondaryClaimAmount > 0 ? x.SecondaryClaimAmount : x.ClaimedAmount,
                ApprovedAmount = x.ApprovedAmount,
                ClaimStatus = x.ClaimStatus,
                SubmittedAt = x.SubmittedAt
            }).ToList();
        }

        public async Task<BenefitClaimDetailDto> GetClaimDetailAsync(Guid id, CancellationToken cancellationToken)
        {
            var claim = await _dbContext.TrxBenefitClaims.AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.BenefitPlan)
                .Include(x => x.Documents.Where(d => !d.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (claim == null)
            {
                throw new KeyNotFoundException($"Klaim dengan ID '{id}' tidak ditemukan.");
            }

            TrxEmployeeBenefitDependent? dependent = null;
            if (claim.DependentId.HasValue)
            {
                dependent = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == claim.DependentId.Value && !x.IsDelete, cancellationToken);
            }

            return new BenefitClaimDetailDto
            {
                Id = claim.Id,
                ClaimNumber = claim.ClaimNumber,
                WorkforceProfileId = claim.WorkforceProfileId,
                EmployeeName = claim.Employee?.FullName ?? claim.WorkforceProfile?.DisplayName ?? "-",
                EmployeeCode = claim.Employee?.EmployeeNumber ?? claim.WorkforceProfile?.ProfileCode ?? "-",
                DependentId = claim.DependentId,
                PatientName = dependent?.DependentName ?? claim.Employee?.FullName ?? claim.WorkforceProfile?.DisplayName ?? "-",
                RelationshipType = dependent?.RelationshipType ?? "Self",
                BenefitPlanName = claim.BenefitPlan?.BenefitPlanName ?? "Asuransi Tambahan RS Quilvian",
                ServiceType = claim.ServiceType ?? claim.ClaimCategory ?? "Inpatient",
                HospitalName = claim.HospitalName ?? claim.ProviderName ?? "-",
                ServiceDate = claim.ServiceStartDate ?? claim.ClaimDate,
                SepBpjsNumber = claim.SepBpjsNumber,
                TotalHospitalBill = claim.TotalHospitalBill > 0 ? claim.TotalHospitalBill : claim.ClaimedAmount,
                PrimaryPayerCoveredAmount = claim.PrimaryPayerCoveredAmount,
                SecondaryClaimAmount = claim.SecondaryClaimAmount > 0 ? claim.SecondaryClaimAmount : claim.ClaimedAmount,
                OutOfPocketExcessAmount = claim.OutOfPocketExcessAmount,
                ApprovedAmount = claim.ApprovedAmount,
                PaidAmount = claim.PaidAmount,
                ClaimStatus = claim.ClaimStatus,
                SubmittedAt = claim.SubmittedAt,
                VerifiedAt = claim.VerifiedAt,
                VerifiedBy = claim.VerifiedByUserId?.ToString(),
                SettledAt = claim.SettledAt ?? claim.PaidAt,
                SettlementReference = claim.SettlementReference ?? claim.PaymentReferenceNumber,
                RejectionReason = claim.RejectionReason,
                Notes = claim.Notes,
                Documents = claim.Documents.Select(d => new ClaimDocumentDetailDto
                {
                    Id = d.Id,
                    DocumentType = d.DocumentType,
                    DocumentNumber = d.DocumentNumber,
                    FilePath = d.FilePath,
                    UploadedAt = d.UploadedAt
                }).ToList()
            };
        }

        public async Task<BenefitClaimDetailDto> CreateEssClaimAsync(
            Guid workforceProfileId, CreateEssClaimRequestDto request, Guid actorUserId, CancellationToken cancellationToken)
        {
            // 1. Validasi Batas Waktu Kadaluwarsa Klaim (VR-BEN-19: maks 60 hari kalender)
            var today = DateTime.UtcNow.Date;
            var serviceDate = request.ServiceDate.Date;
            if ((today - serviceDate).TotalDays > 60)
            {
                throw new InvalidOperationException(
                    $"Pengajuan klaim kedaluwarsa. Batas maksimal pengajuan klaim biaya kesehatan adalah 60 hari sejak tanggal perawatan (VR-BEN-19). Tanggal pelayanan: {serviceDate:yyyy-MM-dd}.");
            }

            // 2. Validasi Mandatory Nomor SEP BPJS Kesehatan pada Rawat Inap / COB (BEN-B5, VR-BEN-16, VR-BEN-17)
            var isInpatient = string.Equals(request.ServiceType, "Inpatient", StringComparison.OrdinalIgnoreCase);
            if (isInpatient || !string.IsNullOrWhiteSpace(request.SepBpjsNumber))
            {
                if (string.IsNullOrWhiteSpace(request.SepBpjsNumber))
                {
                    throw new InvalidOperationException(
                        "Pengajuan klaim COB ditolak. Nomor Surat Eligibilitas Peserta (SEP) BPJS Kesehatan wajib disertakan untuk perawatan rawat inap (VR-BEN-16, BEN-B5).");
                }

                var sep = request.SepBpjsNumber.Trim();
                if (!SepRegex.IsMatch(sep))
                {
                    throw new InvalidOperationException(
                        $"Format nomor SEP tidak valid ('{sep}'). Nomor SEP BPJS Kesehatan wajib 19 karakter alfanumerik (contoh: 0041R0011126V000123) (VR-BEN-17).");
                }
            }

            // 3. Validasi Konsistensi Breakdown Biaya Klaim (VR-BEN-18)
            var calculatedTotal = request.PrimaryPayerCoveredAmount + request.SecondaryClaimAmount + request.OutOfPocketExcessAmount;
            if (Math.Abs(request.TotalHospitalBill - calculatedTotal) > 0.01m)
            {
                throw new InvalidOperationException(
                    $"Perhitungan rincian biaya tidak konsisten. Total tagihan kuitansi (Rp {request.TotalHospitalBill:N2}) wajib sama dengan penjumlahan klaim primer BPJS (Rp {request.PrimaryPayerCoveredAmount:N2}) + sekunder (Rp {request.SecondaryClaimAmount:N2}) + ekses (Rp {request.OutOfPocketExcessAmount:N2}) (VR-BEN-18).");
            }

            // Cari enrollment induk aktif staf
            var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            var enrollmentId = enrollment?.Id ?? Guid.NewGuid();
            var planId = enrollment?.BenefitPlanId ?? Guid.Empty;

            // Nomor urut harian dari provider bersama, bukan angka acak (QBE-CODE-003). Bentuk: CLM-COB-20261110-0001
            var claimNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: BenefitNumberSeries.CobClaimKey,
                Prefix: BenefitNumberSeries.CobClaimPrefix,
                ResetPolicy: BenefitNumberSeries.CobClaimResetPolicy,
                SequenceDigits: BenefitNumberSeries.CobClaimDigits,
                ActorUserId: actorUserId == Guid.Empty ? BenefitNumberSeries.SystemFallbackUserId : actorUserId,
                Instant: DateTimeOffset.UtcNow), cancellationToken);

            var claim = new TrxBenefitClaim
            {
                Id = Guid.NewGuid(),
                ClaimNumber = claimNumber,
                EmployeeBenefitEnrollmentId = enrollmentId,
                WorkforceProfileId = workforceProfileId,
                BenefitPlanId = planId,
                DependentId = request.DependentId,
                ServiceType = request.ServiceType,
                ClaimCategory = request.ServiceType,
                HospitalName = request.HospitalName.Trim(),
                ProviderName = request.HospitalName.Trim(),
                ServiceStartDate = request.ServiceDate,
                ClaimDate = DateTime.UtcNow,
                SepBpjsNumber = request.SepBpjsNumber?.Trim(),
                TotalHospitalBill = request.TotalHospitalBill,
                ClaimedAmount = request.SecondaryClaimAmount,
                PrimaryPayerCoveredAmount = request.PrimaryPayerCoveredAmount,
                SecondaryClaimAmount = request.SecondaryClaimAmount,
                OutOfPocketExcessAmount = request.OutOfPocketExcessAmount,
                EligibleAmount = request.SecondaryClaimAmount,
                ClaimStatus = "Submitted",
                SubmittedAt = DateTime.UtcNow,
                SubmittedByUserId = actorUserId,
                Notes = request.Notes,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.TrxBenefitClaims.Add(claim);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[AUDIT-BEN-08] Pengajuan klaim COB baru '{ClaimNumber}' diajukan oleh staf '{WorkforceProfileId}'. Total: Rp {Total:N0}, Sekunder: Rp {Secondary:N0}.",
                claim.ClaimNumber, workforceProfileId, request.TotalHospitalBill, request.SecondaryClaimAmount);

            return await GetClaimDetailAsync(claim.Id, cancellationToken);
        }

        public async Task<ClaimDocumentDetailDto> AddClaimDocumentAsync(
            Guid claimId, ClaimDocumentUploadRequestDto request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var claim = await _dbContext.TrxBenefitClaims
                .FirstOrDefaultAsync(x => x.Id == claimId && !x.IsDelete, cancellationToken);

            if (claim == null)
            {
                throw new KeyNotFoundException($"Klaim dengan ID '{claimId}' tidak ditemukan.");
            }

            // VR-BEN-21: Format & ukuran file maksimal 5 MB
            if (request.FileSizeBytes > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException("Ukuran berkas melebihi batas maksimal 5 MB (VR-BEN-21).");
            }

            var doc = new TrxBenefitClaimDocument
            {
                Id = Guid.NewGuid(),
                BenefitClaimId = claimId,
                DocumentType = request.DocumentType,
                DocumentName = request.DocumentType,
                DocumentNumber = request.DocumentNumber,
                FilePath = request.FilePath,
                FileSizeBytes = request.FileSizeBytes,
                FileContentType = request.FileContentType,
                VerificationStatus = "Pending",
                UploadedAt = DateTime.UtcNow,
                UploadedByUserId = actorUserId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.TrxBenefitClaimDocuments.Add(doc);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ClaimDocumentDetailDto
            {
                Id = doc.Id,
                DocumentType = doc.DocumentType,
                DocumentNumber = doc.DocumentNumber,
                FilePath = doc.FilePath,
                UploadedAt = doc.UploadedAt
            };
        }

        public async Task<BenefitClaimDetailDto> VerifyClaimAsync(
            Guid claimId, VerifyClaimRequestDto request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var claim = await _dbContext.TrxBenefitClaims
                .FirstOrDefaultAsync(x => x.Id == claimId && !x.IsDelete, cancellationToken);

            if (claim == null)
            {
                throw new KeyNotFoundException($"Klaim dengan ID '{claimId}' tidak ditemukan.");
            }

            var isApproval = string.Equals(request.Status, "Approved", StringComparison.OrdinalIgnoreCase);
            var isRejection = string.Equals(request.Status, "Rejected", StringComparison.OrdinalIgnoreCase);

            if (!isApproval && !isRejection)
            {
                throw new InvalidOperationException("Status keputusan verifikasi tidak valid. Pilihan hanya 'Approved' atau 'Rejected'.");
            }

            if (isRejection)
            {
                if (string.IsNullOrWhiteSpace(request.RejectionReason))
                {
                    throw new InvalidOperationException("Alasan penolakan klaim wajib diisi secara rinci.");
                }

                claim.ClaimStatus = "Rejected";
                claim.RejectionReason = request.RejectionReason.Trim();
                claim.VerifiedAt = DateTime.UtcNow;
                claim.VerifiedByUserId = actorUserId;
                claim.UpdateDateTime = DateTime.UtcNow;
                claim.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "[AUDIT-BEN-09] Klaim COB '{ClaimNumber}' DITOLAK oleh user '{UserId}'. Alasan: {Reason}",
                    claim.ClaimNumber, actorUserId, request.RejectionReason);

                return await GetClaimDetailAsync(claimId, cancellationToken);
            }

            var approvedAmount = request.ApprovedAmount ?? claim.SecondaryClaimAmount;
            claim.ClaimStatus = "Approved";
            claim.ApprovedAmount = approvedAmount;
            claim.RejectionReason = null;
            claim.VerifiedAt = DateTime.UtcNow;
            claim.VerifiedByUserId = actorUserId;
            claim.UpdateDateTime = DateTime.UtcNow;
            claim.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[AUDIT-BEN-09] Klaim COB '{ClaimNumber}' DISETUJUI sebesar Rp {ApprovedAmount:N0} oleh user '{UserId}'.",
                claim.ClaimNumber, approvedAmount, actorUserId);

            return await GetClaimDetailAsync(claimId, cancellationToken);
        }

        public async Task<BenefitClaimDetailDto> SettleClaimAsync(
            Guid claimId, SettleClaimRequestDto request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var claim = await _dbContext.TrxBenefitClaims
                .FirstOrDefaultAsync(x => x.Id == claimId && !x.IsDelete, cancellationToken);

            if (claim == null)
            {
                throw new KeyNotFoundException($"Klaim dengan ID '{claimId}' tidak ditemukan.");
            }

            if (string.IsNullOrWhiteSpace(request.SettlementReference))
            {
                throw new InvalidOperationException("Nomor referensi bukti pelunasan transfer bank wajib diisi.");
            }

            claim.ClaimStatus = "Settled";
            claim.PaidAmount = request.ApprovedSecondaryAmount;
            claim.SettlementReference = request.SettlementReference.Trim();
            claim.PaymentReferenceNumber = request.SettlementReference.Trim();
            claim.SettledAt = DateTime.UtcNow;
            claim.PaidAt = DateTime.UtcNow;
            claim.PaidByUserId = actorUserId;
            claim.UpdateDateTime = DateTime.UtcNow;
            claim.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[AUDIT-BEN-10] Settlement pelunasan klaim '{ClaimNumber}' selesai sebesar Rp {Amount:N0} (Ref: '{Ref}'). Ekses dialihkan ke payroll: {Delegate}.",
                claim.ClaimNumber, request.ApprovedSecondaryAmount, request.SettlementReference, request.DelegateExcessToPayroll);

            return await GetClaimDetailAsync(claimId, cancellationToken);
        }

        public async Task<bool> TerminateBenefitEnrollmentAsync(
            Guid enrollmentId, TerminateBenefitRequestDto request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var insEnrollment = await _dbContext.TrxEmployeeInsuranceEnrollments
                .FirstOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDelete, cancellationToken);

            if (insEnrollment == null)
            {
                throw new KeyNotFoundException($"Kepesertaan asuransi dengan ID '{enrollmentId}' tidak ditemukan.");
            }

            // BEN-B7, VR-BEN-23: Penonaktifan benefit pada tanggal efektif terminasi
            insEnrollment.EnrollmentStatus = "Terminated";
            insEnrollment.TerminationDate = request.EffectiveTerminationDate;
            insEnrollment.TerminationReason = request.TerminationReason;
            insEnrollment.UpdateDateTime = DateTime.UtcNow;
            insEnrollment.UpdateBy = actorUserId;

            // Sinkronkan ke enrollment induk
            var parent = await _dbContext.TrxEmployeeBenefitEnrollments
                .FirstOrDefaultAsync(x => x.Id == insEnrollment.EmployeeBenefitEnrollmentId && !x.IsDelete, cancellationToken);
            if (parent != null)
            {
                parent.EnrollmentStatus = "Terminated";
                parent.EffectiveEndDate = request.EffectiveTerminationDate;
                parent.UpdateDateTime = DateTime.UtcNow;
                parent.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[AUDIT-BEN-11] Benefit enrollment '{EnrollmentId}' diterminasi per tanggal '{Date:yyyy-MM-dd}'. Alasan: {Reason}",
                enrollmentId, request.EffectiveTerminationDate, request.TerminationReason);

            return true;
        }

        public async Task<ExitClearanceMedicalReceivableDto> CheckExitClearanceMedicalReceivableAsync(
            Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var workforce = await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete, cancellationToken);

            var name = workforce?.DisplayName ?? "Staf";

            // Cek transaksi mutasi pemakaian kasir yang memiliki ekses tagihan belum lunas
            // atau klaim yang statusnya masih terbuka dengan ekses mandiri
            var claimsWithExcess = await _dbContext.TrxBenefitClaims.AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete
                    && x.OutOfPocketExcessAmount > 0 && x.ClaimStatus != "Settled")
                .ToListAsync(cancellationToken);

            var totalUnpaidExcess = claimsWithExcess.Sum(x => x.OutOfPocketExcessAmount);
            var isBlocked = totalUnpaidExcess > 0m;

            var unpaidBills = claimsWithExcess.Select(c => new UnpaidBillItemDto
            {
                RegistrationNumber = c.ClaimNumber,
                ServiceDate = c.ServiceStartDate ?? c.ClaimDate,
                GrossAmount = c.TotalHospitalBill,
                ExcessAmount = c.OutOfPocketExcessAmount
            }).ToList();

            if (isBlocked)
            {
                _logger.LogWarning(
                    "[INT-HR-64] Persetujuan Exit Clearance untuk staf '{WorkforceProfileId}' DIBLOKIR karena masih memiliki tunggakan medis sebesar Rp {Total:N0} (VR-BEN-24, BEN-B8).",
                    workforceProfileId, totalUnpaidExcess);
            }

            return new ExitClearanceMedicalReceivableDto
            {
                WorkforceProfileId = workforceProfileId,
                EmployeeName = name,
                HasUnpaidMedicalBill = isBlocked,
                UnpaidExcessAmount = totalUnpaidExcess,
                IsClearanceBlocked = isBlocked,
                UnpaidBills = unpaidBills
            };
        }
    }
}
