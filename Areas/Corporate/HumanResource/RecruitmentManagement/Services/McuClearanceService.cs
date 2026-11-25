using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class McuClearanceService : IMcuClearanceService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly AccessPermissionService? _permissionService;
        private readonly NumberSeriesAllocator? _numberSeriesAllocator;

        public McuClearanceService(
            ApplicationDbContext dbContext,
            AccessPermissionService? permissionService = null,
            NumberSeriesAllocator? numberSeriesAllocator = null)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _permissionService = permissionService;
            _numberSeriesAllocator = numberSeriesAllocator;
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        private async Task<bool> IsAuthorizedMedicalStaffAsync(ClaimsPrincipal user)
        {
            if (_permissionService != null)
            {
                return await _permissionService.HasAccessAsync(user, "OccupationalHealthClearance", "Evaluate");
            }

            return user.HasClaim("Permission", "OccupationalHealthClearance:Evaluate");
        }

        public async Task<RecruitmentResult<PagedResult<McuClearanceListItemResponse>>> GetPagedAsync(
            McuClearanceQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdPreEmploymentMedicalChecks
                .Include(m => m.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Candidate)
                .Include(m => m.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Vacancy)
                .Where(m => !m.IsDelete)
                .AsQueryable();

            if (query.VacancyId.HasValue)
            {
                dbQuery = dbQuery.Where(m => m.Offer != null && m.Offer.Application != null && m.Offer.Application.VacancyId == query.VacancyId.Value);
            }

            if (query.ClearanceResult.HasValue)
            {
                dbQuery = dbQuery.Where(m => m.ClearanceResult == query.ClearanceResult.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim().ToLower();
                dbQuery = dbQuery.Where(m =>
                    m.ReferralNumber.ToLower().Contains(term) ||
                    m.ClinicOrFacility.ToLower().Contains(term) ||
                    (m.Offer != null && m.Offer.OfferNumber.ToLower().Contains(term)) ||
                    (m.Offer != null && m.Offer.Application != null && m.Offer.Application.Candidate != null &&
                     m.Offer.Application.Candidate.FullName.ToLower().Contains(term)));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);
            var items = await dbQuery
                .OrderByDescending(m => m.ReferralDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(m => new McuClearanceListItemResponse
                {
                    Id = m.Id,
                    OfferId = m.OfferId,
                    OfferNumber = m.Offer != null ? m.Offer.OfferNumber : "-",
                    CandidateName = m.Offer != null && m.Offer.Application != null && m.Offer.Application.Candidate != null
                        ? m.Offer.Application.Candidate.FullName : "-",
                    VacancyTitle = m.Offer != null && m.Offer.Application != null && m.Offer.Application.Vacancy != null
                        ? m.Offer.Application.Vacancy.Title : "-",
                    ReferralNumber = m.ReferralNumber,
                    ReferralDate = m.ReferralDate,
                    CheckDate = m.CheckDate,
                    ClinicOrFacility = m.ClinicOrFacility,
                    ClearanceResult = m.ClearanceResult,
                    ExaminingDoctorName = m.ExaminingDoctorName
                })
                .ToListAsync(cancellationToken);

            var totalPages = query.PageSize > 0 ? (int)Math.Ceiling((double)totalCount / query.PageSize) : 0;

            var result = new PagedResult<McuClearanceListItemResponse>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = totalPages,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return RecruitmentResult<PagedResult<McuClearanceListItemResponse>>.Ok(result, "Daftar rujukan MCU berhasil diambil.");
        }

        public async Task<RecruitmentResult<McuClearanceResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var mcu = await _dbContext.HrdPreEmploymentMedicalChecks
                .Include(m => m.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Candidate)
                .Include(m => m.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Vacancy)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDelete, cancellationToken);

            if (mcu == null)
            {
                return RecruitmentResult<McuClearanceResponse>.NotFound("Data rujukan pemeriksaan kesehatan (MCU) tidak ditemukan.");
            }

            var isMedical = await IsAuthorizedMedicalStaffAsync(user);

            var response = new McuClearanceResponse
            {
                Id = mcu.Id,
                OfferId = mcu.OfferId,
                OfferNumber = mcu.Offer?.OfferNumber ?? "-",
                CandidateId = mcu.Offer?.Application?.CandidateId ?? Guid.Empty,
                CandidateName = mcu.Offer?.Application?.Candidate?.FullName ?? "-",
                CandidateNik = RecruitmentJourneyPolicy.MaskNationalIdNumber(mcu.Offer?.Application?.Candidate?.NationalIdNumber),
                VacancyTitle = mcu.Offer?.Application?.Vacancy?.Title ?? "-",
                ReferralNumber = mcu.ReferralNumber,
                ReferralDate = mcu.ReferralDate,
                CheckDate = mcu.CheckDate,
                ClinicOrFacility = mcu.ClinicOrFacility,
                ClearanceResult = mcu.ClearanceResult,
                // Aturan Bisnis HRD-DEC-010 & AT-HRD-D2-09: Masking diagnosa medis & sertifikat untuk non-pemeriksa medis
                DoctorNotes = isMedical
                    ? mcu.DoctorNotes
                    : "[DIAGNOSA MEDIS DIISOLASI - HANYA KESIMPULAN KELAIKAN KERJA DAPAT DIAKSES PIHAK HR/NON-MEDIS]",
                IsDoctorNotesMasked = !isMedical,
                CertificateFilePath = isMedical ? mcu.CertificateFilePath : null,
                ExaminingDoctorName = mcu.ExaminingDoctorName,
                CreateDateTime = mcu.CreateDateTime
            };

            return RecruitmentResult<McuClearanceResponse>.Ok(response, "Detail hasil MCU pra-kerja berhasil diambil.");
        }

        public async Task<RecruitmentResult<McuClearanceResponse>> RegisterMcuAsync(
            RegisterMcuRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.Application)
                    .ThenInclude(a => a!.Candidate)
                .FirstOrDefaultAsync(o => o.Id == request.OfferId && !o.IsDelete, cancellationToken);

            if (offer == null)
            {
                return RecruitmentResult<McuClearanceResponse>.NotFound("Surat penawaran kerja tidak ditemukan.");
            }

            if (offer.Application != null && RecruitmentJourneyPolicy.IsTerminalApplication(offer.Application, offer.Application.Candidate))
            {
                return RecruitmentResult<McuClearanceResponse>.Unprocessable(
                    "Rujukan MCU ditolak karena lamaran atau kandidat berada pada status terminal/gugur.");
            }

            // Aturan Bisnis VR-REC-17 & HRD-DEC-078: Rujukan MCU hanya untuk kandidat yang sudah ConditionalAccepted
            if (offer.Status != JobOfferStatus.ConditionalAccepted)
            {
                return RecruitmentResult<McuClearanceResponse>.Unprocessable(
                    $"Rujukan MCU pra-kerja hanya dapat diterbitkan setelah kandidat menyetujui penawaran bersyarat (ConditionalAccepted). Status penawaran saat ini: '{offer.Status}' (VR-REC-17, HRD-DEC-078).");
            }

            var existingCheck = await _dbContext.HrdPreEmploymentMedicalChecks
                .FirstOrDefaultAsync(m => m.OfferId == request.OfferId && !m.IsDelete, cancellationToken);

            if (existingCheck != null)
            {
                return RecruitmentResult<McuClearanceResponse>.Conflict("Rujukan MCU pra-kerja sudah pernah diterbitkan untuk penawaran ini.");
            }

            var dateCode = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, RecruitmentJourneyPolicy.JakartaTimeZone)
                .ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var mcuPrefix = $"MCU-{dateCode}-";
            var existingMcuNumbers = await _dbContext.HrdPreEmploymentMedicalChecks
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(m => m.ReferralNumber.StartsWith(mcuPrefix))
                .Select(m => m.ReferralNumber)
                .ToListAsync(cancellationToken);

            var referralNumber = await RecruitmentJourneyPolicy.AllocateSequenceAsync(
                _numberSeriesAllocator,
                existingMcuNumbers,
                "HRD_MCU_REFERRAL",
                "MCU",
                userId,
                cancellationToken);

            var mcu = new HrdPreEmploymentMedicalCheck
            {
                Id = Guid.NewGuid(),
                OfferId = request.OfferId,
                ReferralNumber = referralNumber,
                ReferralDate = DateTime.UtcNow,
                CheckDate = request.ScheduledCheckDate,
                ClinicOrFacility = request.ClinicOrFacility.Trim(),
                ClearanceResult = McuClearanceResult.Pending,
                DoctorNotes = request.Notes?.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdPreEmploymentMedicalChecks.AddAsync(mcu, cancellationToken);

            if (offer.Application != null)
            {
                offer.Application.CurrentStage = ApplicationStage.MedicalCheckup;
                offer.Application.StageStatus = "McuReferralIssued";
                offer.Application.UpdateDateTime = DateTime.UtcNow;
                offer.Application.UpdateBy = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(mcu.Id, user, cancellationToken);
        }

        public async Task<RecruitmentResult<McuClearanceResponse>> RecordResultAsync(
            Guid id,
            RecordMcuResultRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            if (request.ClearanceResult == McuClearanceResult.Pending)
            {
                return RecruitmentResult<McuClearanceResponse>.Unprocessable(
                    "Hasil MCU wajib ditetapkan sebagai Fit, FitWithRestrictions, atau Unfit.");
            }

            var mcu = await _dbContext.HrdPreEmploymentMedicalChecks
                .Include(m => m.Offer)
                    .ThenInclude(o => o!.Application)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDelete, cancellationToken);

            if (mcu == null)
            {
                return RecruitmentResult<McuClearanceResponse>.NotFound("Data rujukan MCU tidak ditemukan.");
            }

            if (mcu.ClearanceResult == McuClearanceResult.Unfit)
            {
                return RecruitmentResult<McuClearanceResponse>.Conflict(
                    "Hasil MCU berstatus UNFIT bersifat final dan tidak dapat diubah kembali.");
            }

            var hasHandoff = await _dbContext.HrdCandidateHiringHandoffs
                .AnyAsync(h => h.OfferId == mcu.OfferId && !h.IsDelete && h.Status != HandoffStatus.Cancelled, cancellationToken);
            if (hasHandoff)
            {
                return RecruitmentResult<McuClearanceResponse>.Conflict(
                    "Hasil MCU tidak dapat diubah karena proses serah terima (Handoff) sudah dijalankan.");
            }

            var offer = mcu.Offer;
            if (offer == null || (offer.Status != JobOfferStatus.ConditionalAccepted
                                  && offer.Status != JobOfferStatus.FinalOfferSent
                                  && offer.Status != JobOfferStatus.FinalAccepted))
            {
                return RecruitmentResult<McuClearanceResponse>.Unprocessable(
                    "Hasil MCU tidak dapat dicatat karena status penawaran kerja tidak aktif atau belum ConditionalAccepted.");
            }

            mcu.ClearanceResult = request.ClearanceResult;
            mcu.CheckDate = request.CheckDate;
            mcu.ExaminingDoctorName = request.ExaminingDoctorName.Trim();
            mcu.DoctorNotes = request.DoctorNotes?.Trim();
            mcu.CertificateFilePath = request.CertificateFilePath?.Trim();
            mcu.UpdateDateTime = DateTime.UtcNow;
            mcu.UpdateBy = userId;

            var app = offer.Application;

            // Aturan Bisnis Kriteria 3: Jika UNFIT -> Tawaran kerja otomatis Revoked (Batal Demi Hukum)
            if (request.ClearanceResult == McuClearanceResult.Unfit)
            {
                offer.Status = JobOfferStatus.Revoked;
                offer.RowVersion = Guid.NewGuid();
                offer.UpdateDateTime = DateTime.UtcNow;
                offer.UpdateBy = userId;

                if (app != null)
                {
                    app.CurrentStage = ApplicationStage.Rejected;
                    app.StageStatus = "MCU_UNFIT_OFFER_REVOKED";
                    app.Notes = "Penawaran kerja dibatalkan otomatis oleh sistem karena hasil pemeriksaan kesehatan pra-kerja dinyatakan Tidak Laik Kerja (UNFIT).";
                    app.UpdateDateTime = DateTime.UtcNow;
                    app.UpdateBy = userId;
                }
            }
            else if (request.ClearanceResult == McuClearanceResult.Fit)
            {
                offer.Status = JobOfferStatus.FinalAccepted;
                offer.RowVersion = Guid.NewGuid();
                offer.UpdateDateTime = DateTime.UtcNow;
                offer.UpdateBy = userId;

                if (app != null)
                {
                    app.StageStatus = "McuFitReadyForFinalOfferOrHandoff";
                    app.Notes = "Pemeriksaan kesehatan pra-kerja lulus (FIT).";
                    app.UpdateDateTime = DateTime.UtcNow;
                    app.UpdateBy = userId;
                }
            }
            else if (request.ClearanceResult == McuClearanceResult.FitWithRestrictions)
            {
                // Jangan melompat ke FinalAccepted tanpa mekanisme konfirmasi penyesuaian penugasan/restriksi kerja
                if (app != null)
                {
                    app.StageStatus = "MCU_FIT_WITH_RESTRICTIONS_PENDING_CONFIRMATION";
                    app.Notes = "Pemeriksaan kesehatan pra-kerja lulus dengan catatan pembatasan kerja (FIT WITH RESTRICTIONS); membutuhkan konfirmasi penyesuaian kerja sebelum handoff.";
                    app.UpdateDateTime = DateTime.UtcNow;
                    app.UpdateBy = userId;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(mcu.Id, user, cancellationToken);
        }
    }
}
